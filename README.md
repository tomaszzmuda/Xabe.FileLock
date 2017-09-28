# Xabe.FileLock  
[![Build Status](https://travis-ci.org/tomaszzmuda/Xabe.FileLock.svg?branch=master)](https://travis-ci.org/tomaszzmuda/Xabe.FileLock)
[![NuGet version](https://badge.fury.io/nu/Xabe.FileLock.svg)](https://badge.fury.io/nu/Xabe.FileLock)
[![GitHub issues](https://img.shields.io/github/issues/tomaszzmuda/Xabe.FileLock.svg)](https://github.com/tomaszzmuda/Xabe.FileLock/issues)
[![GitHub stars](https://img.shields.io/github/stars/tomaszzmuda/Xabe.FileLock.svg)](https://github.com/tomaszzmuda/Xabe.FileLock/stargazers)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/tomaszzmuda/Xabe.FileLock/master/LICENSE.md)
[![HitCount](http://hits.dwyl.io/tomaszzmuda/Xabe.FileLock.svg)](http://hits.dwyl.io/tomaszzmuda/Xabe.FileLock)

.NET Standard library providing exclusive lock on file.

## Using ##

Install the [Xabe.FileLock NuGet package](https://www.nuget.org/packages/Xabe.FileLock "") via nuget:

	PM> Install-Package Xabe.FileLock
	
Creating file lock:

	ILock fileLock = new FileLock(file);
	fileLock.Acquire(TimeSpan.FromSeconds(15), true);
	
This will create lock file with extension ".lock" in the same directory. Example: "/tmp/data.txt" -> "/tmp/data.lock".

Last parameter is optional and defines if lock should be automatically refreshing before expired.

If file already has lock file, and it time haven't expired, method returns false.

## Recommended using ##

	ILock fileLock = new FileLock(file);
	if(fileLock.Acquire(TimeSpan.FromSeconds(15), true))
	{
		using(fileLock)
		{
			// file operations here
		}
	}
	
## Lincence ## 

Xabe.FileLock is licensed under MIT - see [License](LICENSE.md) for details.
