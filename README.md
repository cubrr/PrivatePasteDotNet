# PrivatePasteDotNet
#### A C# .NET wrapper for uploading pastes to PrivatePaste.com

## Documentation
See https://cubrr.github.io/PrivatePasteDotNet

## Usage

``` c#
using PrivatePasteDotNet;

public static void Main(string[] args)
{
    Test();
}

private static async void Test()
{
    PrivatePasteResponse response = await PrivatePasteUploader.CreatePaste("Testing PrivatePasteUploader .NET", "C#", true, "60 m", "toastbox");
    System.Windows.Forms.MessageBox.Show(response.PasteUrl);
}

```
