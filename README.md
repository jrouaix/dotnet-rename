## dotnet-rename

A global tool to reduce pain when moving/renaming projects repository wide.

An attemp to fill the need for https://github.com/dotnet/project-system/issues/3511

### Warning /!\

This tool can fail ! It has no atomic/transactional behavior so it can leave a big mess in your project/solution if it fails in the middle of a rename. It's by design !

**BEFORE USING IT** : commit your work !

### Installation

Installation is very easy. Just run this command and the tool will be installed. 

`dotnet tool install --global Dotnet.Rename`

### Usage

In your repository folder:

To move project MyLib/MyLib.csproj to src/My.Lib/My.Lib.csproj:

~/git/dotnet-rename/_sample/$ `dotnet-rename SampleLib/SampleLib.csproj Sample.Lib -s src`

### Limitations

Didn't get namespace renaming (solutions wide) ... contributions are welcome or any sugestion on the way to achieve that properly.

### Update

`dotnet tool update -g Dotnet.Rename`

### Uninstall

`dotnet tool uninstall -g Dotnet.Rename`
