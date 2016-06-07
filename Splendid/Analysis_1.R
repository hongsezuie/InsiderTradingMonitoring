Tickers <- data.frame(read.csv("~/Projects/C#/Splendid/Splendid/bin/Release/Tickers.txt", sep = "|", stringsAsFactors = FALSE, as.is = TRUE))
PriceData <- data.frame(read.csv("~/Projects/C#/Splendid/Splendid/bin/Release/PriceData.txt", sep = "|", stringsAsFactors = FALSE, as.is = TRUE))
TradeData <- data.frame(read.csv("~/Projects/C#/Splendid/Splendid/bin/Release/TradingData.txt", sep = "|",  stringsAsFactors = FALSE, as.is = TRUE))

PriceData$Date <- as.Date(PriceData$Date)
TradeData$Transaction.Date <- as.Date(TradeData$Transaction.Date)
TradeData$Price.per.Share <- as.numeric(gsub("[\\$\\,]", "", TradeData$Price.per.Share))
TradeData$Transaction.Shares <- as.numeric(gsub("[\\,]", "", TradeData$Transaction.Shares))

Sys.time()

pdf(paste0("~/Projects/C#/Splendid/Splendid/bin/Release/", "ITR", Sys.time(), ".pdf"), width = 12, height = 8)
for(i in c(1:dim(Tickers)[1])){

  ticker <- Tickers[i,][,1]
  
  date_since <- '2013-01-01'
  
  buysell_resolution <- 10000
  
  FilteringMask <- PriceData$Ticker==ticker&PriceData$Date>=date_since
  
  MaximumPrice <- max(PriceData$Adj.Close[FilteringMask]) * 1.1
  
  MinimumPrice <- min(PriceData$Adj.Close[FilteringMask]) / 1.3
  
  if(sum(FilteringMask) > 0)
  {
    plot(PriceData[FilteringMask,"Date"], 
         PriceData[FilteringMask, "Close"],
         type = "l", col = "black", 
         main = ticker, xlab = "Date", ylab = "Close",
         ylim = c(MinimumPrice, MaximumPrice))
    
    BuyMask <- TradeData$Buy.Sell=="Buy" & TradeData$Ticker==ticker & TradeData$Transaction.Date>=date_since
    SellMask <- TradeData$Buy.Sell=="Sell" & TradeData$Ticker==ticker & TradeData$Transaction.Date>=date_since
    
    points(TradeData$Transaction.Date[BuyMask], TradeData$Price.per.Share[BuyMask], pch = 3, cex = TradeData$Transaction.Shares[BuyMask]/buysell_resolution, col = "green")
    points(TradeData$Transaction.Date[SellMask], TradeData$Price.per.Share[SellMask], pch = 4, cex = TradeData$Transaction.Shares[SellMask]/buysell_resolution, col = "red")
  }
}
dev.off()
