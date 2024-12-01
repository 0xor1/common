using Cocona;
using Common.Shared;
using Microsoft.Extensions.Logging;

namespace Common.Cli;

public class Dnsk
{
    private static readonly Key DnskKey = new("dnsk");
    private readonly ILogger<Dnsk> _log;

    public Dnsk(ILogger<Dnsk> log)
    {
        _log = log;
    }

    [Command("dnsk")]
    public async Task Run([Argument(Description = "The path to the parent directory where code repos are stored")] string reposPath, [Argument(Name="key",Description="The Key of the new repo")] Key key)
    {
        var dnskPath = Path.Join(reposPath, DnskKey.ToString());
        var newPath = Path.Join(reposPath, key.ToString());
        Throw.OpIf(!Directory.Exists(dnskPath), $"{dnskPath} directory doesn't exists.");
        Throw.OpIf(Directory.Exists(newPath), $"{newPath} directory already exists.");
        Directory.CreateDirectory(newPath);
        await CopyDir(dnskPath, newPath, key, false);
    }

    private async Task CopyDir(string src, string dst, Key key, bool isProj = true)
    {
        _log.LogInformation("Copying {Src} to {Dst}", src, dst);
        _log.LogInformation("Creating Directory {Dst}", dst);
        Directory.CreateDirectory(dst);
        Directory.GetFiles(src).ForEach(async file =>
        {
            var fileName = ReplaceDnsk(Path.GetFileName(file), key);
            var dstFile = Path.Join(dst, fileName);
            _log.LogInformation("Copying {File} to {DstFile}", file, dstFile);
            if (file.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}"))
            {
                // for the git repo files just do direct copy
                File.Copy(file, dstFile);
                return;
            }
            var content = await File.ReadAllTextAsync(file);
            content = ReplaceDnsk(content, key);
            await File.WriteAllTextAsync(dstFile, content);
        });
        Directory.GetDirectories(src).Where(x => !isProj || x.Split(Path.DirectorySeparatorChar).Last() is not ("bin" or "obj")).ForEach(
            async dir =>
            {
                var dirName = dir.Split(Path.DirectorySeparatorChar).Last();
                await CopyDir(dir, Path.Join(dst, ReplaceDnsk(dirName, key)), key, dirName.StartsWith($"{DnskKey.ToPascal()}."));
            });
    }

    private static string ReplaceDnsk(string src, Key key)
        => src.Replace(DnskKey.ToPascal(), key.ToPascal()).Replace(DnskKey.ToCamel(), key.ToCamel());
}