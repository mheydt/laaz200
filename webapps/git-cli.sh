az group create -n webapps -l westus
az appservice plan create -n githubdeployas -g webapps --sku B1 --is-linux
az webapp create -n laaz200githubdeploy -g webapps --plan githubdeployas --runtime "DOTNETCORE|2.1"
az webapp deployment source config -n laaz200githubdeploy -g webapps --repo-url https://github.com/mheydt/AspNetCoreAzWebAppSample --branch master --manual-integration

az webapp deployment source show -n laaz200githubdeploy -g webapps
az webapp show -n laaz200githubdeploy -g webapps
az webapp show -n laaz200githubdeploy -g webapps --query "defaultHostName" -o tsv
