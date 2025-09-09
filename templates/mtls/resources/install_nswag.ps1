<#
.SYNOPSIS
    Installs or updates the NSwag.Console .NET global tool.

.DESCRIPTION
    This script installs NSwag.Console if it's not present,
    or updates it if it is already installed. Finally, it verifies the installation
    by displaying the version number.

.NOTES
    Requires the .NET SDK to be installed on the system.
#>

# --- Configuration ---
$ToolName = "NSwag.ConsoleCore"

# --- Main Logic ---
try {
    Write-Host "Checking for existing installation of '$ToolName'..."

    # Check if the tool is already installed
    $installedTools = dotnet tool list --global
    
    if ($installedTools -like "*$($ToolName.ToLower())*") {
        # If installed, update it to the latest version
        Write-Host "'$ToolName' is already installed. Updating to the latest version..."
        dotnet tool update --global $ToolName
    } else {
        # If not installed, install it
        Write-Host "'$ToolName' not found. Installing now..."
        dotnet tool install --global $ToolName
    }

    # Verify the installation by checking the version
    Write-Host "Installation/Update complete. Verifying..."
    nswag version

} catch {
    Write-Error "An error occurred. Please ensure the .NET SDK is installed and in your PATH."
    Write-Error $_.Exception.Message
}