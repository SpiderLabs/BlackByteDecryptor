# BlackByteDecryptor

This is a decryptor for the ransomware BlackByte. The key is stored in a file called forest.png, which was downloaded from http[:]//45.9.148.114/forest.png

## Requirements

This requires .NET core 3.1 runtime which you can download from here: https://dotnet.microsoft.com/download/dotnet/3.1

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

## Pre-built binary

We suggest building the source yourself, but if you prefer a pre-built binary, you can download it [here](build/BlackByteDecryptor.zip). We have also provided a sample encrypted file called [spider.png.blackbyte](sample/spider.png.blackbyte).

## Write up and analysis

[BlackByte Ransomware – Pt. 1 In-depth Analysis](https://www.trustwave.com/en-us/resources/blogs/spiderlabs-blog/blackbyte-ransomware-pt-1-in-depth-analysis/)

[BlackByte Ransomware – Pt 2. Code Obfuscation Analysis](https://www.trustwave.com/en-us/resources/blogs/spiderlabs-blog/blackbyte-ransomware-pt-2-code-obfuscation-analysis/)
