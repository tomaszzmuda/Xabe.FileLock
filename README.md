# Xabe.FileLock  [![Build Status](https://travis-ci.org/tomaszzmuda/Xabe.FileLock.svg?branch=master)](https://travis-ci.org/tomaszzmuda/Xabe.FileLock)

Simple .net core library providing exclusive lock on file.

## Using ##

Install the [Xabe.FileLock NuGet package](https://www.nuget.org/packages/Xabe.FileLock "") via nuget:

	PM> Install-Package Xabe.FileLock
	
Creating file lock:

	ILock fileLock = FileLock.Acquire(file, TimeSpan.FromSeconds(15), true);
	
This will create lock file with extension ".lock" in the same directory. Example: "/tmp/data.txt" -> "/tmp/data.lock".

Last parameter is optional and defines if lock should be automatically refreshing before expired.

If file already has lock file, and it time haven't expired, method returns null.

## Recommended using ##

	using(ILock fileLock = FileLock.Acquire(file, TimeSpan.FromSeconds(15), true))
	{
		// file operations here
	}
	
## Lincence ## 

Xabe.FileLock is licensed under MIT - see [License](License.md) for details.
