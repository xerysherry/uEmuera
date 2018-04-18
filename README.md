uEmuera
=======

Emuera是Emulator of Eramaker的缩写，是Windows平台下文字游戏平台。

该项目为Emuera的Unity3D移植版本。意在利用Unity3D多平台特性，方便移植到非Windows平台。

当前项目以至于emuera1821+v8版本源代码。

在Mi4C测试通过，几乎可以执行所有era脚本游戏！

下载
----

[https://github.com/xerysherry/uEmuera/releases](https://github.com/xerysherry/uEmuera/releases)

如何使用：
--------

1. 请确保era相关文件编码为UTF8，包括\*.csv, \*.ERB, \*.ERH, __\*.config__。__特别注意\*.config文件，这个文件请一定保证编码为UTF8。__  是否BOM头无所谓。

2. 请在初次运行app时，选择允许“文件访问”的权限。

3. 请把处理完毕的era脚本文件夹放置在sdcard下的emuera文件夹内。完整路径为storage/emulated/0/emuera, storage/emulated/1/emuera, storage/emulated/2/emuera

已知问题/需要改进项：
-------------------

1. 当前版本为实现图片显示

2. 无法在app内修改era游戏配置

3. 无调试功能

4. 部分游戏的某些指令效率较低，导致卡顿

5. 可能会比较耗电（Unity3D程序通病）

6. ...

截图
----

开始界面
![Screenshot1](Screenshot/screenshot1.png)
游戏运行界面
![Screenshot1](Screenshot/screenshot2.png)
快捷按钮
![Screenshot1](Screenshot/screenshot3.png)
指令输入
![Screenshot1](Screenshot/screenshot4.png)