# mTLS Client Library Template Guide

This repository contains the example code accompanying the OutSystems Developer Cloud (ODC) documentation for supporting mTLS.

> **Note:** This guide is designed to be used with the official public documentation, which can be found here:
> [**Supporting mTLS in ODC**](https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_custom_code/supporting_mtls_in_odc/)

This README outlines the steps necessary to generate and compile the example code provided in this template.


## 1. Prerequisites: Install NSwag

Before you can generate the client code, you need the NSwag command-line tool. You have two options for installation:

* **Option A (Manual):** Follow the official installation instructions on the [NSwag repository](https://github.com/RicoSuter/NSwag) making sure you install a version that supports .Net 8.
* **Option B (Helper Script):** Run the PowerShell helper script included in this template:

    ```powershell
    .\install_nswag.ps1
    ```
    This script will use npm to install the latest version of NSwag and update NSwag to use .Net 8 as the default runtime.


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
> `System.InvalidOperationException: The specified runtime in the document (Net80) differs from the current process runtime (Net90). Change the runtime with the '/runtime:Net80' parameter or run the file with the correct command line binary.`

#### **Solution:**

You will need to update the .Net binaries being used by NSwag to point to the version of .Net you are using.

1.  **Update your NSwag .Net 8 binaries** by running the following command:
    ```powershell
    nswag version /runtime:Net80
    ```

2.  **Update the runtime property.** Change the value of `"runtime"` to ensure the version matches that which you found in step 1. For example, if `nswag version` showed `Net80`, your configuration file should look like this:
    ```json
    "runtime": "Net80"
    ```