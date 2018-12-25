az group create -n dml -l westus
az storage account create -g dml -l westus -n laaz200dmlprimary
az storage account create -g dml -l eastus -n laaz200dmlsecondary
