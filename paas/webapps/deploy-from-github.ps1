$rg = "webapps"
$planname = "githubdeployas"
$appname = "laaz200githubdeploy"
$repourl = "https://github.com/mheydt/AspNetCoreAzWebAppSample"
az group create -n $rg -l westus
az appservice plan create -n $planname -g $rg --sku B1 --is-linux
az webapp create -n $appname -g $rg --plan $planname --runtime "DOTNETCORE|2.1"
az webapp deployment source config -n $appname -g $rg --repo-url $repourl --branch master --manual-integration

az webapp deployment source show -n $appname -g $rg
az webapp show -n $appname -g $rg
az webapp show -n $appname -g $rg --query "defaultHostName" -o tsv
az webapp deployment source sync -n $appname -g $rg
az group delete -n $rg --yes
