#library loading
library(readr)
library(astsa)
library(fGarch)
library(aTSA)
library(forecast)
library(rjson)
library(rstudioapi)

#set this directory as the root
setwd(dirname(rstudioapi::getActiveDocumentContext()$path))

#data reading
rawData = read_csv("Results.csv")
Results=rawData[747:9387,]
Results[!duplicated(Results$timestamp), ]
attach(Results)

#plot the orignal data
png("1.png", width = 800, height = 600, pointsize=26)
par(mar=c(4,4,2,2))
plot(last,type='l')
dev.off()

#make a return
n = length(last)
lastReturn=((last[-1]/last[-n])-1)*1000

#adf test
adf.test(last,1)
adf.test(lastReturn,1)

#acf and pacf
png("2.png", width = 800, height = 1000, pointsize=26)
par(mfrow=c(2,1),mar=c(4,4,2,2))
acf(lastReturn,lag.max = 30 , ylim=c(-0.05,0.1),main='')
pacf(lastReturn,lag.max = 30 , ylim=c(-0.05,0.1),main='')
dev.off()

#auto.arima model selection
auto.arima(lastReturn,start.p = 0,start.q = 0,max.p = 6,max.q = 6,allowmean = FALSE,trace=TRUE,stepwise = FALSE)

#sarima and its output for all possible models
png("4.1.png", width = 800, height = 800, pointsize=26)
par(mar=c(4,4,2,2))
arma200=sarima(lastReturn,2,0,3)
dev.off()
png("4.2.png", width = 800, height = 1000, pointsize=26)
par(mar=c(4,4,2,2))
arma200=sarima(lastReturn,4,0,1)
dev.off()
png("4.3.png", width = 800, height = 800, pointsize=26)
par(mar=c(4,4,2,2))
arma200=sarima(lastReturn,5,0,0)
dev.off()
png("4.4.png", width = 800, height = 800, pointsize=26)
par(mar=c(4,4,2,2))
arma200=sarima(lastReturn,0,0,5)
dev.off()
png("4.5.png", width = 800, height = 800, pointsize=26)
par(mar=c(4,4,2,2))
arma200=sarima(lastReturn,2,0,0)
dev.off()

#residuals
res203=arma203$fit$residuals
res401=arma401$fit$residuals
res005=arma005$fit$residuals
res500=arma500$fit$residuals

#garch model initial setting
garch203 = as.list(NULL)
garch401 = as.list(NULL)
garch005 = as.list(NULL)
garch500 = as.list(NULL)

#for loop to create all possible arma+garch model from P,Q=1,1 to 4,3
i = 1
for (P in 1:4){
  for (Q in 1:3){
    paste(print(P),print(Q))
    try( garch203[[i]] <- eval(parse(text = paste("garchFit( ~ arma(2,3) + garch(", P ,", ", Q , "), data = lastReturn, trace = FALSE, cond.dist='std',include.shape=TRUE )" ) )), silent = TRUE)
    try( garch401[[i]] <- eval(parse(text = paste("garchFit( ~ arma(4,1) + garch(", P ,", ", Q , "), data = lastReturn, trace = FALSE, cond.dist='std',include.shape=TRUE )" ) )), silent = TRUE)
    try( garch005[[i]] <- eval(parse(text = paste("garchFit( ~ arma(0,5) + garch(", P ,", ", Q , "), data = lastReturn, trace = FALSE, cond.dist='std',include.shape=TRUE )" ) )), silent = TRUE)
    try( garch500[[i]] <- eval(parse(text = paste("garchFit( ~ arma(5,0) + garch(", P ,", ", Q , "), data = lastReturn, trace = FALSE, cond.dist='std',include.shape=TRUE )" ) )), silent = TRUE)
    i = i + 1 
  } 
}

#plot the histogram of the original data
png("5.png", width = 800, height = 500, pointsize=26)
par(mar=c(4,4,2,2))
hist(lastReturn,breaks="scott")
dev.off()

#garch model selection
garch203=Filter(Negate(function(x) is.null(unlist(x))), garch203)
garch401=Filter(Negate(function(x) is.null(unlist(x))), garch401)
garch005=Filter(Negate(function(x) is.null(unlist(x))), garch005)
garch500=Filter(Negate(function(x) is.null(unlist(x))), garch500)
garch203opt = Reduce(function(x,y) if(x@fit$ics[2] < y@fit$ics[2]){x} else{y}, garch203)
garch401opt = Reduce(function(x,y) if(x@fit$ics[2] < y@fit$ics[2]){x} else{y}, garch401)
garch005opt = Reduce(function(x,y) if(x@fit$ics[2] < y@fit$ics[2]){x} else{y}, garch005)
garch500opt = Reduce(function(x,y) if(x@fit$ics[2] < y@fit$ics[2]){x} else{y}, garch500)

#plot the residuals
png("6.png", width = 800, height = 600, pointsize=26)
par(mar=c(4,4,2,2))
plot(garch401opt@residuals-res203,type='l',ylim=c(-0.15,0.15),ylab='Difference of residual')
dev.off()

#compare the residuals
png("7.png", width = 800, height = 1250, pointsize=26)
par(mfrow=c(3,1),mar=c(4,4,2,2))
plot(res401,type='l',ylab='residual',xlab='time',main='ARMA')
plot(garch401opt@residuals,type='l',ylab='residual',xlab='time',main='ARMA+GARCH')
plot(garch401opt@residuals-res401,type='l',ylim=c(-0.15,0.15),ylab='diff.',xlab='time',main='Difference')
dev.off()

#histogram of the residuals
png("8.png", width = 800, height = 500, pointsize=26)
par(mar=c(4,4,2,2))
hist(garch401opt@residuals,freq = FALSE,main='Histogram',xlab="residuals")
x = seq(-6, 6, length=240)
hx = dt(x,garch401opt@fit$coef[10])
lines(x,hx)
dev.off()

#qqplot of ARMA and ARMA+GARCH models
png("9.png", width = 1000, height = 600, pointsize=26)
par(mfrow=c(1,2),mar=c(4,4,2,2))
qqnorm(arma401$fit$residuals,main='ARMA')
qqline(arma401$fit$residuals)
plot(qt(ppoints(garch401opt@residuals),garch401opt@fit$coef[10]), sort(garch401opt@residuals),xlab='Theoretical Quantiles',ylab='Sample Quantiles',main='ARMA+GARCH')
qqline(ppoints(garch401opt@residuals))
dev.off()

#prediction
png("10.png", width = 800, height = 600, pointsize=22)
par(mar=c(4,4,2,2))
predict(garch401opt,plot=TRUE,nx=75,n.ahead=15,mse='uncond')
dev.off()

#plot of BCH price
png("11.png", width = 800, height = 600, pointsize=26)
par(mar=c(4,4,2,2))
plot(last_BT,type='l')
dev.off()

#compare the prices
png("12.png", width = 800, height = 1000, pointsize=26)
par(mfrow=c(2,1),mar=c(4,4,2,2))
plot(last,type='l',ylab='BTC/JPY')
plot(last_BT,type='l',ylab='BCH/USD')
dev.off()

#ccf of the two prices
png("13.png", width = 800, height = 600, pointsize=26)
par(mar=c(4,4,2,2))
ccf=ccf(last_BT,last,xlim=c(-5,5))
plot(ccf$lag,ccf$acf,type="p",xlab='lag',ylab='Correlation')
dev.off()

#scatterplot of the two prices
png("14.png", width = 1200, height = 900, pointsize=34)
par(mar=c(4,4,2,2))
lag2.plot(lag(last_BT,3),last)
dev.off()

#read a json data that contains data of closed trade
json_data = fromJSON(file = "./coincheck.json")

#data transform
stack <- data.frame(matrix(rep(1, 7*350), ncol=7))
colnames(stack) <- c("created_at","closed_at","open_rate","closed_rate","side","amount","pl")
for (i in 1:length(json_data[[1]])){
  stack$created_at[351-i]=as.numeric(as.POSIXct(json_data[[1]][[i]]$created_at,format="%Y-%m-%dT%H:%M:%S"))
  stack$closed_at[351-i]=as.numeric(as.POSIXct(json_data[[1]][[i]]$closed_at,format="%Y-%m-%dT%H:%M:%S"))
  stack$open_rate[351-i]=as.numeric(json_data[[1]][[i]]$open_rate)
  stack$closed_rate[351-i]=as.numeric(json_data[[1]][[i]]$closed_rate)
  stack$side[351-i]=if(json_data[[1]][[i]]$side=="buy"){0.5}else{-0.5}
  stack$amount[351-i]=as.numeric(json_data[[1]][[i]]$amount)
  stack$pl[351-i]=as.numeric(json_data[[1]][[i]]$pl)
}
stack2=stack[stack$created_at%%10<3 & stack$closed_at%%10<3 & stack$closed_at<1510659070+60*60*8,]

#show profit/loss
png("15.png", width = 800, height = 400, pointsize=26)
par(mar=c(4,4,2,2))
plot(cumsum(stack2$pl)*100/(stack2$amount[1]*stack2$open_rate[1]/5),type='l',ylab='Profit(%)',xlab='Trade Index')
dev.off()

#show buy/sell points
png("16.png", width = 800, height = 500, pointsize=26)
par(mar=c(4,4,2,2))
plot(timestamp,last,type="l",xlab='Timestamp (sec.)',ylab='BTC/JPY')
lines(stack2$created_at-60*60*8,stack2$open_rate,type="p",cex=1.5,lwd=2,col=2.5+stack2$side)
lines(stack2$closed_at-60*60*8,stack2$closed_rate,type="p",cex=1.5,lwd=2,col=2.5-stack2$side)
dev.off()
