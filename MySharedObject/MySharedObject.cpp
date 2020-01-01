#include "MySharedObject.h"

#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "MySharedObject", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "MySharedObject", __VA_ARGS__))

extern "C" {
	int GetMyInt() {
		return 321;
	}

	/* ���̒P���Ȋ֐��́A���̃_�C�i�~�b�N �l�C�e�B�u ���C�u�������R���p�C������Ă���v���b�g�t�H�[�� ABI ��Ԃ��܂��B*/
	const char* getPlatformABI()
	{
#if defined(__arm__)
#if defined(__ARM_ARCH_7A__)
#if defined(__ARM_NEON__)
#define ABI "armeabi-v7a/NEON"
#else
#define ABI "armeabi-v7a"
#endif
#else
#define ABI "armeabi"
#endif
#elif defined(__i386__)
#define ABI "x86"
#else
#define ABI "unknown"
#endif
		LOGI("This dynamic shared library is compiled with ABI: %s", ABI);
		return "This native library is compiled with ABI: %s" ABI ".";
	}




	/* ���̒P���Ȋ֐��́A���̃_�C�i�~�b�N �l�C�e�B�u ���C�u�������R���p�C������Ă���v���b�g�t�H�[�� ABI ��Ԃ��܂��B*/
	const char * MySharedObject::getPlatformABI()
	{
		return getPlatformABI();
	}

	int MySharedObject::getInt()
	{
		return 1234;
	}

	void MySharedObject()
	{
	}

	MySharedObject::MySharedObject()
	{
	}

	MySharedObject::~MySharedObject()
	{
	}

	bool SetStringToCharArraySub(char* dest, int destLength, const char* src, int srcLength) {
		if (destLength < 1) return true;

		bool err;
		int copyLength, nullIndex;

		if (destLength + 1 >= srcLength) {
			// �S�����܂�p�^�[��
			err = false;
			copyLength = srcLength;
			nullIndex = srcLength;
		}
		else {
			// �S�����܂�Ȃ��p�^�[��
			err = true;
			copyLength = destLength - 1;
			nullIndex = destLength - 1;
		}

		for (int i = 0; i < copyLength; i++) {
			dest[i] = src[i];
		}
		dest[nullIndex] = '\0';
		return err;
	}

	bool SetStringToCharArray(char* dest, int destLength, const char* src) {
		return SetStringToCharArraySub(dest, destLength, src, (int)strlen(src));
	}

	// string(Out) �\�񕶎���
	bool GetString(char* dest, int destLength) {
		//return SetStringToCharArray(dest, destLength, "Hello, I'm Library!");
		return SetStringToCharArray(dest, destLength, getPlatformABI());
	}

}
