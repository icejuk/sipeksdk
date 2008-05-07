SIPek project!


Q: How to build pjsip's Cpp - dotNet Wrapper inside MS Visual Studio?

A: Include pjsipDll project (pjsipDll.vcproj) into pjproject solution! 
   Set project dependencies (without *_test and pjsua). Build pjsipdll project 
   and wait for pjsipdll.dll.


Q: How to use TLS with openser?

A: First put certificate and private key created by openser to application folder 
   and rename them to server.crt and pkey.key! 
   Check option TLS in Sipek->Settings.
   
   
Q: What to do if something goes wrong?

A: Check configuration, network status, or see the sipek discussions for similar problems.
   Finaly analize pjsip.log file by yourself or send question to sipek discussion group.

  

Good luck!
Sasa