#library(RODBC) # you can write the results to a database by using this library
args = commandArgs(trailingOnly = TRUE) # allows R to get parameters
#cat(as.numeric(args[1])+5)# converts 3 to a number (numeric)

outputsR<-c(args[1],args[2],args[3])
outputsR
  
  

  
