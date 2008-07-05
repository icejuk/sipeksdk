SIPek project!

See http://sipekphone.googlepages.com/pjsipwrapper!


5th July 2008: 
	PjsipDll.dll is built with pjsip version 0.9.0 and with TLS transport (openssl libraries needed!)
        Old version (0.8.0) is renamed to pjsipDll_v0.8.0.dll.
	Version without TLS support is built as pjsipDll_notls.dll.


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