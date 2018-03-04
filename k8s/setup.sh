#! /bin/sh
# https://docs.microsoft.com/en-us/azure/container-service/kubernetes/container-service-kubernetes-windows-walkthrough

# Creat Resource Group
az group create --name morshe-k8s-drones --location westeurope

# Create Cluster
az aks create --orchestrator-type kubernetes --resource-group morshe-k8s-drones --name morshe-k8s-drones --service-principal 4856f1fc-6060-4916-a541-47cf138c1bbb --client-secret /vUcloiV+xABtlSLhD28G7vW4U4dWuPQEXIPDD3d/jA= --generate-ssh-keys --node-vm-size Standard_DS3_v2 --node-count 2 -k 1.8.1

# Must for first time only ; Install Kubectl CLI. If you are using Windows than kubectl is in program files (x86). Make sure it is in your PATH variable
az aks kubernetes install-cli

# Connect kubectl to cluster
az aks kubernetes get-credentials --resource-group=morshe-k8s-drones --name=morshe-k8s-drones

# Install and initialize helm
apt-get helm
helm init

# Proxy to the dashboard
kubectl proxy