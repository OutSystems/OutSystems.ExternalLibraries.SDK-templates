[CmdletBinding()]
param (
	[Parameter(Mandatory=$true)]
	[string]$swaggerlocation,
	[string]$namespace = "mTLSclient",
	[string]$classname = "ExampleClient",
	[string]$clientbaseclass = "ExampleClientBase",
	[string]$templatedirectory = "nswagtemplates"
) 

$configfile = Get-Content -path MySwaggerConfig.nswag.template -Raw

if($swaggerlocation.ToLower().StartsWith("http")) {
	$configfile =  $configfile.Replace('{{swaggerurl}}',$swaggerlocation)
} else {
	$jsonstring = (Get-Content -path .\swagger.json -Raw | ConvertTo-Json | ConvertFrom-Json).value | ConvertTo-Json
	$configfile =  $configfile.Replace('{{swaggerurl}}','').
							   Replace('{{swaggerjson}}',$jsonstring)
}

$configfile =  $configfile.Replace('{{namespace}}',$namespace).
						   Replace('{{classname}}',$classname).
						   Replace('{{clientbaseclass}}',$clientbaseclass).
						   Replace('{{templatedirectory}}',$templatedirectory)
echo "Write NSwag config file MySwaggerConfig.nswag"
Set-Content -Path MySwaggerConfig.nswag -value $configfile

if(![bool] (Get-Command -ErrorAction Ignore -Type Application nswag)) {
	echo "NSwag not found please install it from https://github.com/RicoSuter/NSwag"
	return
}

echo "Generating client files with NSwag"
nswag run /runtime:Net80