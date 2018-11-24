

# STA525-CPP
Paperwork is available [here](https://portfolium.com/entry/time-series-analysis-in-cryptocurrency-markets).

# About
**Compare the price of BTC and BCH, and gain profit!**

In fall 2017, there was an abrupt and drastic increase in the price of Bitcoin Cash (BCH), reaching approximately 40% of the price of Bitcoin (BTC) in early November. At that time, it was a effective strategy to just compare the price of BTC and BCH to make buy/sell decisions in BTC trading, since the change in BCH price has significantly been leading the opposite change in the other price (see the links attached in top).

To make an automatic trading based on this tendency, we made an .NET C# application which fetched BTC price from Coincheck.com and BCH price from Bittrex.com every 10 seconds and executed buy/sell decisions in BTC trading based on the change in BCH price. The price data was stored on Azure SQL Datasbase and later investigated by an R program. 

# Programs
## .NET C#
Main program set that contains trading logic, database model, and some basic files.

.NET Framework version: 4.6.1 
Nuget packages: Quartz, Bittrex.Net, Mathnet.Numerics and more
 
## R
In "R" folder. Data analysis part.

Methodologies: ARMA+GARCH time series analysis, cross-correlation function and more

# Notes
## Installation
To run the .NET C# program, you need to download the C# api for Coincheck.com from [this repository](https://github.com/coincheckjp/coincheck-cs) (as it has no license) and extract it to the root directory.

## Misc
This project is originally done as the final project of STA525 (Fall 2017) at California State Polytechnic University, Pomona and submitted in Fall 2017 quarter.

We believe this method is already obsolete and futile to gain a significant profit until BCH become a competitor of BTC again someday, so please find another good way if you are in a rush :)
