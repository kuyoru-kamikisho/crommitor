哦，这个仓库的名字怎么叫crommitor？没事不必在意，起名是个令人头疼的事，而且这只是一个控制台程序，所以完全没必要在乎这仓库的名字。

因为常规的changelog生成工具用起来感觉不是很好用，格式也不是自己所需要的，因此写了这个小工具（造轮子造惯了）。

如何使用？

这个exe文件接受以下参数：

   options params
   -c      [config file path] support relative path & absolute path
    
配置文件的名称随意

## 近期计划（可能）

- 也许一些人更喜欢使用json，所以可以尝试支持读取JSON配置文件
- 如果使用了cz-git，那意味着要写两遍提交类别，所以可以支持读取cz-git的配置文件

## License

[MIT](https://opensource.org/licenses/MIT)

Copyright (c) 2013-present, Yuxi (Evan) You