rem --- add "-sr localmachine" to add to local computer store
makecert -iv IkosaTest.pvk -ic ikosatest.cer -cy end -pe -n CN="%1" -eku 1.3.5.1.5.5.7.3.1 -ss my -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12
rem --- add generated certificate to Trusted People store so that PeerTrust (PeerOrChainTrust) works
rem --- make sure client endpoints reference the "LocalTrust" endpoint-behavior configuration