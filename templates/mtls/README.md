# mTLS Client Library Template Guide

This repository contains the example code accompanying the OutSystems Developer Cloud (ODC) documentation for supporting mTLS.

> **Note:** This guide is designed to be used with the official public documentation, which can be found here:
> [**Supporting mTLS in ODC**](https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_custom_code/supporting_mtls_in_odc/)

This README outlines the steps necessary to generate and compile the example code provided in this template.


## 1. Prerequisites: Install NSwag

Before you can generate the client code, you need the NSwag command-line tool. You have two options for installation:

* **Option A (Manual):** Follow the official installation instructions on the [NSwag repository](https://github.com/RicoSuter/NSwag) making sure you install a version that supports .NET 8 or .NET 10.
* **Option B (Helper Script):** Run the PowerShell helper script included in this template:

    ```powershell
    .\install_nswag.ps1
    ```
    This script will use npm to install the latest version of NSwag and update NSwag to use .NET 10 as the default runtime.


## 2. Generating and Building the Client Library

Follow these steps to generate the `ExampleClient` code and build the final library.

1.  **Generate Client Code**
    Run the `generate_clientcode.ps1` PowerShell script to create the client file from the OpenAPI specification.
    ```powershell
    .\generate_clientcode.ps1
    ```

2.  **Copy the Generated File**
    Move the newly generated `ExampleClient.cs` file into the `mTLSClientLib` directory.

3.  **Build the Solution**
    Build the `mTLSClientLib` solution using your preferred IDE (like Visual Studio) or the `dotnet build` command.



##  Troubleshooting

### Runtime Mismatch Error

If you encounter the following error message while running the generation script, it means the .NET runtime version expected by NSwag doesn't match the one specified in the configuration.

> **Error:**
> `System.InvalidOperationException: The specified runtime in the document (Net100) differs from the current process runtime (Net80). Change the runtime with the '/runtime:Net100' parameter or run the file with the correct command line binary.`

#### **Solution:**

You will need to update the .NET binaries being used by NSwag to match the version of .NET you are targeting (`Net80` for .NET 8, `Net100` for .NET 10).

1.  **Update your NSwag .NET binaries** by running the following command (adjust the runtime token to match your target):
    ```powershell
    nswag version /runtime:Net100
    ```
    Use `/runtime:Net80` if you are targeting .NET 8.

2.  **Update the runtime property.** Change the value of `"runtime"` in `MySwaggerConfig.nswag` to match the token used in step 1. For example, if targeting .NET 10, your configuration file should look like this:
    ```json
    "runtime": "Net100"
    ```
    For .NET 8, use `"runtime": "Net80"` instead.