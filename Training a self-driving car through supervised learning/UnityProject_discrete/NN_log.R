logdata <- read.csv('log.txt', header=F)
colnames(logdata)<- c("sensor0","sensor1","sensor2","sensor3","sensor4","turn")
logdata$turn<-as.factor(logdata$turn)

m <- model.matrix( 
  ~ sensor0 + sensor1 + sensor2 + sensor3 + sensor4 + turn, 
  data = logdata 
)
index <- sample(1:nrow(m),round(0.75*nrow(m)))
train_ <- m[index,]
test_ <- m[-index,]
library(neuralnet)
n <- names(train_)
f <- as.formula(turn0 + turn1 ~ sensor0 + sensor1 + sensor2 + sensor3 + sensor4)
nn <- neuralnet(f,data=train_,hidden=5,act.fct = "logistic", linear.output = FALSE, lifesign = "full",lifesign.step = 1000,stepmax=1e6, threshold=0.5)
plot(nn)
pr.nn <- compute(nn,test_[,2:6])
pr.nn_<-1*(pr.nn$net.result>0.5)+0*(pr.nn$net.result<0.5)
test.r <- test_[,c("turn0","turn1")]

pr.nn.tr <- compute(nn,train_[,2:6])
pr.nn.tr_<-1*(pr.nn.tr$net.result>0.5)+0*(pr.nn.tr$net.result<0.5)
train.r <- train_[,c("turn0","turn1")]

#turn0 confusion matrix test
turn0_pred_1=c(sum(pr.nn_[,1]==1&test.r[,1]==1),sum(pr.nn_[,1]==1&test.r[,1]==0))
turn0_pred_0=c(sum(pr.nn_[,1]==0&test.r[,1]==1),sum(pr.nn_[,1]==0&test.r[,1]==0))
turn0_confusionmx=data.frame(turn0_pred_1,turn0_pred_0)
row.names(turn0_confusionmx)=c("true_1","true_0")

#turn1 confusion matrix test
turn1_pred_1=c(sum(pr.nn_[,2]==1&test.r[,2]==1),sum(pr.nn_[,2]==1&test.r[,2]==0))
turn1_pred_0=c(sum(pr.nn_[,2]==0&test.r[,2]==1),sum(pr.nn_[,2]==0&test.r[,2]==0))
turn1_confusionmx=data.frame(turn1_pred_1,turn1_pred_0)
row.names(turn1_confusionmx)=c("true_1","true_0")

#turn0 confusion matrix train
tr.turn0_pred_1=c(sum(pr.nn.tr_[,1]==1&train.r[,1]==1),sum(pr.nn.tr_[,1]==1&train.r[,1]==0))
tr.turn0_pred_0=c(sum(pr.nn.tr_[,1]==0&train.r[,1]==1),sum(pr.nn.tr_[,1]==0&train.r[,1]==0))
tr.turn0_confusionmx=data.frame(tr.turn0_pred_1,tr.turn0_pred_0)
row.names(tr.turn0_confusionmx)=c("true_1","true_0")

#turn1 confusion matrix train
tr.turn1_pred_1=c(sum(pr.nn.tr_[,2]==1&train.r[,2]==1),sum(pr.nn.tr_[,2]==1&train.r[,2]==0))
tr.turn1_pred_0=c(sum(pr.nn.tr_[,2]==0&train.r[,2]==1),sum(pr.nn.tr_[,2]==0&train.r[,2]==0))
tr.turn1_confusionmx=data.frame(tr.turn1_pred_1,tr.turn1_pred_0)
row.names(tr.turn1_confusionmx)=c("true_1","true_0")

#accuracy of turn1 and turn0
accuracy.turn1.te<-(turn1_confusionmx[1,1]+turn1_confusionmx[2,2])/sum(turn1_confusionmx)
accuracy.turn1.tr<-(tr.turn1_confusionmx[1,1]+tr.turn1_confusionmx[2,2])/sum(tr.turn1_confusionmx)
accuracy.turn0.te<-(turn0_confusionmx[1,1]+turn0_confusionmx[2,2])/sum(turn0_confusionmx)
accuracy.turn0.tr<-(tr.turn0_confusionmx[1,1]+tr.turn0_confusionmx[2,2])/sum(tr.turn0_confusionmx)

#precision and recall of turn1
prec.turn1.te<-turn1_confusionmx[1,1]/(turn1_confusionmx[1,1]+turn1_confusionmx[2,1])
recall.turn1.te<-turn1_confusionmx[1,1]/(turn1_confusionmx[1,1]+turn1_confusionmx[1,2])


##Save workspace
save(nn,file="trainedNN.RData")

###Not useful for classification
MSE.nn <- sum((test.r - pr.nn_)^2)/nrow(test_)

