from dotenv import load_dotenv
load_dotenv();
import pandas as pd
import os
from sqlalchemy import create_engine
import pymysql
import collections
pymysql.install_as_MySQLdb()

user=os.environ.get("USER") 
password=os.environ.get("PWD")
database=os.environ.get("DB")
host=os.environ.get("HOST")
port=os.environ.get("PORT")

con: str = f"mysql+pymysql://{user}:{password}@{host}:{port}/{database}"
sqlEngine = create_engine(con, pool_recycle=3600)
dbConnection    = sqlEngine.connect()

CONSUMERS = ["neutron", "sputnik", "gopher", "hero"]
PROVIDER = "provider"

def getConsumerEndBlock(chain: str, end_date: str = '2022-11-30') -> int:
    #query = f"SELECT max(height) FROM {chain}_blocks WHERE time_date = '{end_date}' ORDER BY height DESC;"
    query = f"SELECT max(height) FROM {chain}_blocks WHERE DATE_ADD(time, INTERVAL 1 hour) <= '{end_date}' ORDER BY height DESC"
    response = dbConnection.execute(query)
    return response.fetchone()[0]

def getConsumerStartTimestamp(chain: str) -> str:
    query = f"SELECT DATE_ADD(time, INTERVAL 1 hour) FROM {chain}_blocks WHERE height = 1;"
    response = dbConnection.execute(query)
    return response.fetchone()[0]

def getProviderStartBlockFromConsumerStartTime(start_time: str) -> int:
    #query = f"SELECT min(height) FROM provider_blocks WHERE time_date >= '{start_time}' ORDER BY height ASC;"
    query = f"SELECT min(height) FROM provider_blocks WHERE DATE_ADD(time, INTERVAL 1 hour) >= '{start_time}' ORDER BY height ASC;"
    response = dbConnection.execute(query)
    return response.fetchone()[0]

def getProviderEndBlockFromConsumerStartTime(end_date: str = '2022-11-30') -> int:
    query = f"SELECT max(height) FROM provider_blocks_signatures WHERE timestamp <= '{end_date}' AND timestamp != '0001-01-01' ORDER BY height DESC;"
    #query = f"SELECT max(height) FROM provider_blocks WHERE time_date = '{end_date}' ORDER BY height DESC;"
    response = dbConnection.execute(query)
    return response.fetchone()[0]

def getValidatorUpdatesFromProvider(start_block: str, end_block: str) -> pd.DataFrame:
    query = f"""SELECT min(height) as height, valset, max(timestamp) as timestamp
                FROM (SELECT asd.height                             as height,
                            sha(concat(group_concat((address)), group_concat(voting_power))) as valset,
                            DATE_ADD(time, INTERVAL 1 hour)                                  as timestamp
                        FROM (SELECT height, address, voting_power FROM provider_block_validators
                            WHERE height > {start_block} AND height < {end_block}
                            ORDER BY id ASC) asd
                        LEFT JOIN provider_blocks sb ON sb.height = asd.height
                        GROUP BY height) xyz
                GROUP BY valset
                ORDER BY height ASC;"""
    return pd.read_sql(query, dbConnection)

def getConsumerChainValidatorSetChanges(chain: str, end_block: int) -> pd.DataFrame:
    query_sputnik = f"""SELECT min(height) as height, valset, max(timestamp) as timestamp
        FROM (SELECT asd.height                             as height,
                    sha(concat(group_concat((address)), group_concat(voting_power))) as valset,
                    DATE_ADD(time, INTERVAL 1 hour)                                  as timestamp
                FROM (SELECT height, address, voting_power FROM {chain}_block_validators
                    WHERE height > 0 AND height < {end_block}
                    ORDER BY id ASC) asd
                LEFT JOIN {chain}_blocks sb ON sb.height = asd.height
                GROUP BY height) xyz
        GROUP BY valset
        ORDER BY height ASC;
        """
    return pd.read_sql(query_sputnik, dbConnection)

def inconsistencyCheck() -> pd.DataFrame:
    df_inconsistent = pd.DataFrame(columns=df_consumer.columns)
    for _, row in df_consumer.iterrows():
        # save if current consumer valset has not seen in provider valsets from launch till now
        inPreviousSets = row.valset in list(df_provider[(df_provider.timestamp < row.timestamp)].valset)
        inLaterSets = row.valset in list(df_provider[(df_provider.timestamp > row.timestamp)].valset)

        if inPreviousSets == False and inLaterSets == False:
        #if row.valset not in list(df_provider[(df_provider.timestamp < row.timestamp)].valset):
            df_inconsistent = df_inconsistent.append(row)
    
    return df_inconsistent

def valsets(consumer_chain: str, consumer_height: int, provider_height: int) -> bool:
    query_consumer = f"SELECT address FROM {consumer_chain}_block_validators WHERE height = {consumer_height} ORDER BY id ASC;"
    query_provider = f"SELECT address FROM provider_block_validators WHERE height = {provider_height} ORDER BY id ASC;"
    df_consumer  = pd.read_sql(query_consumer, dbConnection)
    df_provider  = pd.read_sql(query_provider, dbConnection)
    
    return list(df_consumer.address), list(df_provider.address)

def checkOrdering(chain: str) -> None: 
    with open(f"{chain}_wrong_ordering.txt", "w+") as writer:
        for _, row in df_inconsistencies.iterrows():
            # get all vsc from provider with valcount
            prov = df_provider[(df_provider.timestamp < row.timestamp)].sort_values(by="timestamp", ascending=False).head(1)
            if len(prov) == 0 and row.height == 1: continue
            
            output = f"{chain}: {row.height:<6} | {row.timestamp} || provider: {prov.iloc[0].height} | {prov.iloc[0].timestamp}"
            consumer_set, provider_set = valsets(chain, row.height, prov.iloc[0].height)
            diff = None
            if len(consumer_set) != len(provider_set):
                diff = " -> different set length"

            else:
                for i in range(len(consumer_set)):
                    if consumer_set[i] != provider_set[i]:
                        diff = " -> wrong ordering"
                        
            if diff is None:
                diff= " -> vp difference"

            print(output+diff)
            writer.write(output+diff+"\n")

if __name__ == "__main__":
    for chain in CONSUMERS:
        print(f"processing '{chain}'")

        # get timestamp of first consumer block
        consumer_start_time = getConsumerStartTimestamp(chain)

        provider_start_block = getProviderStartBlockFromConsumerStartTime(consumer_start_time)
        provider_end_block = getProviderEndBlockFromConsumerStartTime('2022-11-30')
        print(f"\tstart time: {consumer_start_time} || start block: {provider_start_block} - end block: {provider_end_block}")
        
        # get last block before date
        consumer_end_block = getConsumerEndBlock(chain, '2022-11-30')
        
        # provider chain infos
        print(f"retrieve provider validator sets for consumer chain {chain}")
        df_provider = getValidatorUpdatesFromProvider(provider_start_block, provider_end_block)
        print(f"found {df_provider.shape[0]} for provider")
        df_provider.to_csv(f'{chain}_provider_unique_valsets.csv', index=False)

        print(f"retrieve consumer validator sets for {chain}")
        df_consumer = getConsumerChainValidatorSetChanges(chain, consumer_end_block)
        df_consumer.to_csv(f'{chain}_unique_valsets.csv', index=False)

        # get inconsistencies
        df_inconsistencies = inconsistencyCheck()

        print(f"found {df_inconsistencies.shape[0]} for chain {chain}")
        df_inconsistencies.to_csv(f'{chain}_inconsistent_validator_set_changes.csv', index=False)

        print("check ordering")
        checkOrdering(chain)
