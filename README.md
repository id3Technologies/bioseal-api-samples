# BioSeal API samples

## Introduction

### Content

This repository contains basic samples for **id3 Technologies** BioSeal RESTful API usage in C# and Python languages.

Going through those samples before trying to integrate it in your applications is strongly recommended.

## Getting started

### Step 1: Get an authorization token

To run any of the samples, you need to get an authorization token from **id3 Technologies**. To do so, you must contact us at the following e-mail address: bioseal@id3.eu.

**id3 Technologies** will help you to define your particular BioSeal use case scenario, by discussing together the biographic fields you need in your BioSeal Project Definition document.

Then, once your request will be accepted, you will receive your own token for the WebService, with your use case identifier and version. Once you are here, you can move forward to step 2.


### Step 2: Fill your own information

To run the test program, you will have to replace the variables in <> by your own information:

- Webservice authorization
    - Put your authorization token in the variable _authKey__.

- Use case information
    - Put your use case identifier in the variable _useCaseId_
    - Put your use case version number in the variable _useCaseversion_
    - Edit the _BioSealSamplePayload_ class properties according to the biographics fields defined in your BioSeal Project Definition document.

### Step 3: Test

Once all the parameters are filled, execute the program to launch the generation / verification tests defined in _TestGenerationVerification()_.
