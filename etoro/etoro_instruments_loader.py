# Import the necessary modules
import json
import pandas as pd
import requests

# Define the links for the scraping
instruments_link = 'https://api.etorostatic.com/sapi/app-data/web-client/app-data/instruments-groups.json'
data_link = 'https://api.etorostatic.com/sapi/instrumentsmetadata/V1.1/instruments/bulk?bulkNumber=1&totalBulks=1'

# Gather types of instruments and their attributes
response = requests.get(instruments_link)
parsed_types = json.loads(response.text)

# Divide types
instruments = parsed_types['InstrumentTypes']
exchanges = parsed_types['ExchangeInfo']
stocks = parsed_types['StocksIndustries']
crypto = parsed_types['CryptoCategories']

# Gather all the instruments
response = requests.get(data_link)
data = json.loads(response.text)['InstrumentDisplayDatas']

# We collect the instruments with their attributes here
inst = []

# Loop through all the instruments
for d in data:

    # NEW EDIT: If the instrument is not available for the users, we don't need it
    if d['IsInternalInstrument']:
        continue

    # Gather the necessary data about the instrument
    instrument_typeID = d['InstrumentTypeID']
    name = d['InstrumentDisplayName']
    exchangeID = d['ExchangeID']
    symbol = d['SymbolFull']

    # Instrument type
    instrument_type = next(item for item in instruments
                           if item['InstrumentTypeID'] == instrument_typeID)['InstrumentTypeDescription']

    # Industry type
    try:
        industryID = d['StocksIndustryID']
        industry = next(item for item in stocks
                        if item['IndustryID'] == industryID)['IndustryName']
    # If the instrument don't have industry, we have to give it a placeholder
    except (KeyError, StopIteration):
        industry = '-'

    # Exchange location
    try:
        exchange = next(item for item in exchanges
                        if item['ExchangeID'] == exchangeID)['ExchangeDescription']
    # If the instrument don't have exchange location, we have to give it a placeholder
    except StopIteration:
        exchange = '-'

    # Sum up the gathered data
    inst.append({
        'name': name,
        'symbol': symbol,
        'instrument type': instrument_type,
        'exchange': exchange,
        'industry': industry
    })

# Convert list of dictionaries to a DataFrame
df = pd.DataFrame(inst)

# Save DataFrame to a CSV file
df.to_csv('etoro_list.csv', index=False)
