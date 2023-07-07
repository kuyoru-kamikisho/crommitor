<p align=center><b>简介</b></p>
<p align=center>本程序的核心功能是生成changelog，也围绕该功能包含了一些辅助性的git功能</p>
<p align=center>这个仓库的名字怎么叫crommitor？</p>
<p align=center>不必在意，起名向来是个令人头疼的事，而且这只是一个控制台程序，所以完全没必要在乎这仓库的名字。</P>
<p align=center>为什么要开发这个小工具？常规的changelog生成工具用起来感觉不是很好用，格式也和自己所想的有所差距，因此写了这个小工具。</p>

<hr>

## 参数

这个exe文件可以接受以下参数：

    [options]    [params] <optional> description

    -c           [config file path] <?> support relative path & absolute path.
                 By default, query the config file named csogrc.yml in the directory
                 where the exe file is located. A changelog will be generated based on
                 the configuration file. At the same time, this configuration file
                 is also used by other functions, please refer to the configuration file
                 tutorial for more details.
    -s           If you only want to generate a changelog file, you can append this command,
                 then it will be executed silently, and the cmd window will no longer pop up.
    -v           Show the version.
    -h, --help   Show the help info.

## 配置

默认情况下我们使用主程序所在目录（简称根目录）下的`csogrc.yml`作为程序的配置文件，当您使用 `-c`
参数时，您指定的配置文件将被指定为有效配置文件，下面是关于该配置文件的示例及描述。

### 一个完整的示例文件

```yml
useplatform: gitlab
projpath: E:\Projects\vsapps\commitor
branch: origin/main
description: 这是一个changelog
title: ChangeLog Title
committypes:
  - value: fix
    description: 修复了一些已知的问题或bug
  - value: build
    description: 改变了构建时的行为
  - value: feat
    description: 新增了一些功能特性
  - value: config
    description: 更改了配置文件，并未改变其它源码
  - value: style
    description: 更改了样式文件，并未改变其它源码
  - value: docs
    description: 增加或修改了一些文档，没有修改源码
  - value: refactor
    description: 对部分源码进行了重构，不影响现有功能
onlyusers:
  - value: kamikisho
    description: null
outputdir: ./changelog.md
```

### 字段含义

| Field                      | Type                      | Description |
|----------------------------|---------------------------|-------------|
| branch                     | `string`                  |             |
| committypes                | `<{value,description}>[]` |             |
| committypes[i].value       | `string`                  |             |
| committypes[i].description | `string`                  |             |
| description                | `string`                  |             |
| onlyusers                  | `<{value,description}>[]` |             |
| onlyusers[i].value         | `string`                  |             |
| onlyusers[i].description   | `string`                  |             |
| outputdir                  | `string`                  |             |
| projpath                   | `string`                  |             |
| title                      | `string`                  |             |
| useplatform                | `string`                  |             |

## 对比其他工具

#### cz-git

## 近期计划（可能）

- 实现本文档中定义的功能
- 也许一些人更喜欢使用json，所以可以尝试支持读取JSON配置文件
- 如果使用了cz-git，那意味着要写两遍提交类别，所以可以支持读取cz-git的配置文件
- 不过我都自动生成changelog了，为什么我不写点代码直接踢掉cz-git呢？还能省去配置cz-git的时间。
- 兼容其它平台Mac、unix

## License

[MIT](https://opensource.org/licenses/MIT)

Copyright (c) 2023-present, kuyoru-kamikisyo