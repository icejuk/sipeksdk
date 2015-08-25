# Introduction #

Welcome to the Sipek FAQ...


**How to change transport in SipekSdk (UDP/TCP/TLS)?**
```
To change SIP transport you have to set the TransportMode property from IConfigurationInterface/IAccount. 

Example: Get your instance of IConfigurationInterface, set the appropriate transport mode and then reregister:
 
'Config.Accounts[accountId].TransportMode = ETransportMode.TM_TCP;'

```

**How to enable DNS SRV in SipekSdk?**

```
Just set the nameServer member variable of SipConfigStruct class in pjsipConfig.cs. 
```
> See a short description of how pjsip handles [DNS SRV mechanism](dnssrv.md).
