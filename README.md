# AzureWorkerTCPListerner
Azure worker role configured for multiple tcp clients

Clone or download and run project with F5. Use telnet to connect. Configured for port 8000. To connect locally use:

telnet 127.0.0.1 8000

returns each small letter as upper case

Based on https://bitbucket.org/PrashantPratap/azurecloudservicestcpsocketserver/src/master/ only changed for multiple client and threading.

This server was not stress tested. It´s more a simple sample of doing tcp server in Azure. Best solution would be if function apps support tcp, but now they only support http(s)

