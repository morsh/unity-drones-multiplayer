#! /bin/sh
# https://docs.microsoft.com/en-us/azure/container-service/kubernetes/container-service-kubernetes-windows-walkthrough

# Creat Resource Group
az group create --name morshe-k8s-drones --location westeurope

# Create Cluster
az acs create --orchestrator-type kubernetes --resource-group morshe-k8s-drones --name morshe-k8s-drones --service-principal 4856f1fc-6060-4916-a541-47cf138c1bbb --client-secret /vUcloiV+xABtlSLhD28G7vW4U4dWuPQEXIPDD3d/jA= --generate-ssh-keys --agent-count 1 --master-count 1 --orchestrator-version 1.7

# Must for first time only ; Install Kubectl CLI. If you are using Windows than kubectl is in program files (x86). Make sure it is in your PATH variable
az acs kubernetes install-cli

# Connect kubectl to cluster
az acs kubernetes get-credentials --resource-group=morshe-k8s-drones --name=morshe-k8s-drones

# Proxy to the dashboard
kubectl proxy

# OR:::
kubectl get pods --all-namespaces
# copy *dashboard* name
kubectl port-forward kubernetes-dashboard-N-A-M-E-X-X-X 9090 --namespace kube-system
# browse http://localhost:9090/