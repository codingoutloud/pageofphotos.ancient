# make sure there are entries in both Staging and Production slots

Get-Date
echo "Beginning."

$serviceName = "pageofphotos"

if ($serviceName.Length -eq 0)
{ 
   Write-Host 'Please configure a value for $serviceName'
   exit -1 
}
echo "About to VIP Swap and swap cscfg for $serviceName"

$prodServiceConfigTempFile = "d:\temp\prodServiceConfig.cscfg"
$stagServiceConfigTempFile = "d:\temp\prodServiceConfig.cscfg"

$stageSlotFound = $Null -ne (Get-AzureDeployment -ServiceName $serviceName -ErrorAction SilentlyContinue -Slot Staging)
$prodSlotFound = $Null -ne (Get-AzureDeployment -ServiceName $serviceName -ErrorAction SilentlyContinue -Slot Production)

echo "stageSlotFound? = $stageSlotFound"  
echo "prodSlotFound? = $prodSlotFound"

if ($stageSlotFound -and $prodSlotFound)
{
   #$prodServiceConfigValue = Get-AzureDeployment -ServiceName $serviceName -Slot Production | Select Configuration 
   #$stagServiceConfigValue = Get-AzureDeployment -ServiceName $serviceName -Slot Staging | Select Configuration 

   # Get ServiceConfiguration.cscfg settings from Production and Staging
   $prodServiceConfig = Get-AzureDeployment -ServiceName $serviceName -Slot Production | Select -Expand Configuration > $prodServiceConfigTempFile
   $stagServiceConfig = Get-AzureDeployment -ServiceName $serviceName -Slot Staging | Select -Expand Configuration > $stagServiceConfigTempFile

   # VIP Swap
   Move-AzureDeployment -ServiceName $serviceName

   # Set ServiceConfiguration.cscfg settings to Production and Staging
   Set-AzureDeployment -ServiceName $serviceName -Slot Production -Config -Configuration $prodServiceConfigTempFile
   Set-AzureDeployment -ServiceName $serviceName -Slot Staging -Config -Configuration $stagServiceConfigTempFile

   Get-Date
   echo "Done. Succeeded. VIP Swapped."
}
else
{
   echo "*** Does not run unless both Production and Staging Slots are populated. ***"  
   Get-Date
   echo "Done. Failed. No changes were made."
   exit -1
}
