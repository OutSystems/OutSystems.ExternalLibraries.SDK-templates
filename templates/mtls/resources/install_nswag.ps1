[CmdletBinding()]
param()

$packageName = "nswag"

# Check if npm is available in the system's PATH.
Write-Host "Checking for prerequisites (Node.js and npm)..."
$npmPath = Get-Command npm -ErrorAction SilentlyContinue

if ($null -eq $npmPath) {
    # If npm is not found, inform user and exit.
    Write-Host -ForegroundColor Red "Error: 'npm' command not found."
    Write-Host -ForegroundColor Yellow "Please install Node.js (which includes npm) from https://nodejs.org/"
    Write-Host -ForegroundColor Yellow "After installation, ensure it's in your system's PATH, then re-run this script."

    exit 1
}
else {
    Write-Host -ForegroundColor Green "Success: npm found at $($npmPath.Source)"
    Write-Host "Proceeding with installation..."
    Write-Host ""
}

# Run the npm command to install NSwag globally.
Write-Host "Attempting to install '$packageName' globally via npm. This may take a moment..."
npm install -g $packageName

# Check the exit code of the last command to determine success or failure.
if ($LASTEXITCODE -eq 0) {
    # Success case
    Write-Host -ForegroundColor Green "'$packageName' has been installed successfully!"
    Write-Host ""
    Write-Host "Verifying installation by checking the version and updating to use Net 8 binaries:"
    
    # Update NSWag version to use .Net 8
    nswag version /runtime:Net80
}
else {
    Write-Host -ForegroundColor Red "Installation failed. Please review the npm output above for errors."
    Write-Host -ForegroundColor Yellow "You may need to run PowerShell as an Administrator."
}