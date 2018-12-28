$rgName = "batchRG"
$stgAcctName = "laaz200batchsa"
$location = "southcentralus"
$batchAcctName = "laaz200batchacct"
$poolName = "myPool"

az group create `
 -l $location `
 -n $rgName

az storage account create `
 -g $rgName `
 -n $stgAcctName `
 -l $location `
 --sku Standard_LRS

az batch account create `
 -n $batchAcctName `
 --storage-account $stgAcctName `
 -g $rgName `
 -l $location

az batch account login `
 -n $batchAcctName `
 -g $rgName `
 --shared-key-auth

az storage account keys list `
 -g $rgName -n $stgAcctName `
 --query "[].{value:value}[0].value"

az storage account keys list `
 -g $rgName -n $stgAcctName `
 --query "[0].value" | tr -d '"'

az batch account keys list `
 -g $rgName -n $batchAcctName `
 --query primary 
 
az batch account keys list `
 -g $rgName -n $batchAcctName 

az batch pool delete -n $poolName
az group delete -n $rgName