# SenseNet-File-Size-Report-Generator
This is a simple tool for generating reports about the file sizes in the [SenseNet ECM](https://github.com/SenseNet/sensenet) repository.

It is based on [Sense/Net Client library for .Net](https://github.com/SenseNet/sn-client-dotnet)

# Usage 

The tool collects the number of versions, and summerize the size of these version for each file, in the database.
The tool first query the number of the target elements, and iterates through them in smaller chunks avoiding sql connection time-out.
The results outputted into a .csv file.
The tool can be configured to upload the result to a filepath and send via e-mail.


Below you can find the configuration parameters, with example datas: 

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.2" />
  </startup>
  <appSettings>
    <add key="SendByMail" value="true"/>
    <add key="EmailList" value="john.doe@sensenet.com; jane@doesensenet.com"/>
    <add key="Extensions" value=""/>
    <add key="ContentTypes" value="PreviewImage"/>
    <add key="InTree" value="/Root"/>
    <add key="ExtensionsBlackList" value=""/>
    <add key="ExcludePreviews" value="false"/>
    <add key="UploadNetworkPath" value="true"/>
    <add key="NetworkPath" value="c:\temp"/>
    <add key="ItemsCountInIteration" value="100000"/>
    <add key="SubjectPrefix" value="SNFilesize Test"/>
    <add key="SQLConnectionTimeoutInSeconds" value="150"/>
    <add key="SQLCommandTimeoutInSeconds" value="300"/>

    <add key="SMTP" value="" />
    <add key="SMTPUser" value="" />
    <add key="SMTPPassword" value="" />
    <add key="EmailFrom" value=""/>
  </appSettings>
  <connectionStrings>
    <add name="SnCrMsSql" connectionString="Data Source=.;Initial Catalog=sncr;Integrated Security=True" providerName="System.Data.SqlClient"/>
  </connectionStrings>
</configuration>
```
- **SendByMail**: wether the application should send the report via e-amil
- **EmailList**: list of the target e-mail addresses, separeted by ";"
- **Extensions**: list of the file extensions wich the tool should look for - leave it empty to include every file type
- **ContentTypes**: the list of [content types](http://wiki.sensenet.com/Content_Type) wich the tool should look for - leave it empty to include every content type
- **InTree**: file direction in which the tool should run
- **ExtensionsBlackList**: list of the file extensions which the tool should ignore - leave it empty for not excluding any filetype
- **ExcludePreviews**: whether the tool should ignore preview files
- **UploadNetworkPath**: whether the tool should upload the result file to a given file direction
- **NetworkPath**: the target directory in which the outpout file should be uploaded
- **ItemsCountInIteration**: the number of file items the tool should process in an iteration (it is sufficient to divide the processing in smaller chunks, as the processing of a very large amount of file may cause sql connection time out)
- **SubjectPrefix**: the prefix which should be used in the sent e-mail subkect field
- **SQLConnectionTimeoutInSeconds**: the amount of seconds the tool should wait for the database response on connection
- **SQLCommandTimeoutInSeconds**: the amount of seconds the tool should wait for the database response on query
- **SMTP**: the SMTP connection server address
- **SMTPUser**: the SMTP connection username
- **SMTPPassword**: the SMTP connection user-password
- **EmailFrom**: the sender e-mail address

You can set the database connection parameters in the **connectionString** part.

After you setted the configuration file, you can run the tool without any further paramater: 

```
SNFileSizeReportGenerator.exe 
```


