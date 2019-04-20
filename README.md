## dotnet-rename

A global tool to reduce pain when moving/renaming projects repository wide.

An attemp to fill the need for https://github.com/dotnet/project-system/issues/3511

### Installation

Installation is very easy. Just run this command and the tool will be installed. 

`dotnet tool install --global Dotnet.Rename`

### Usage

In your repository folder:

To move project MyLib/MyLib.csproj to src/My.Lib/My.Lib.csproj:

~/myrepo/$ `dotnet-rename MyProject/MyProject.csproj My.Project -s src`


### Update

`dotnet tool update -g Dotnet.Rename`

### Limitations

Didn't get namespace renaming (solutions wide) ... contributions are welcome or any sugestion on the way to achieve that properly.

