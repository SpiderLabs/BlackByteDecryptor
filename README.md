# BlackByteDecryptor

This is a decryptor for the ransomware BlackByte. The key is stored in a file called forest.png, which was downloaded from http[:]//45.9.148.114/forest.png

## Decrypting an encrypted file

```
> BlackByteDecryptor forest.png spider.png.blackbyte
```

## Decrypting a directory

```
> BlackByteDecryptor forest.png c:\temp
```

This will decrypt files in the c:\temp directory, or to recursively decrypt a directory:

```
> BlackByteDecryptor forest.png c:\temp -r
```
