#pragma once

class MySharedObject
{
public:
	const char* getPlatformABI();
	int getInt();
	MySharedObject();
	~MySharedObject();
};

