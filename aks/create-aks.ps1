$rg = "aksdemos"
$cluster = "akscluster"

az group create -n $rg --location westus
az aks create -g $rg -n $cluster --node-count 1 --enable-addons monitoring --generate-ssh-keys
az aks get-credentials -g $rg -n $cluster
kubectl get nodes
kubectl apply -f aks/azure-vote.yaml
kubectl get service azure-vote-front --watch