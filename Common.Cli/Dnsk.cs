using Cocona;
using Common.Shared;
using Microsoft.Extensions.Logging;

namespace Common.Cli;

public class Dnsk
{
    private const string DnskPascal = "Dnsk";
    private const string DnskCamel = "dnsk";
    private readonly ILogger<Dnsk> _log;

    public Dnsk(ILogger<Dnsk> log)
    {
        _log = log;
    }

    [Command("dnsk")]
    public async Task Run([Argument] string reposPath, [Argument] string pascal, [Argument] string camel)
    {
        var dnskPath = Path.Join(reposPath, DnskPascal);
        var newPath = Path.Join(reposPath, pascal);
        Throw.OpIf(!Directory.Exists(dnskPath), $"{dnskPath} directory doesn't exists.");
        Throw.OpIf(Directory.Exists(newPath), $"{newPath} directory already exists.");
        Directory.CreateDirectory(newPath);
        await CopyDir(dnskPath, newPath, pascal, camel, false);
    }

    private async Task CopyDir(string src, string dst, string pascal, string camel, bool isProj = true)
    {
        _log.LogInformation("Copying {Src} to {Dst}", src, dst);
        _log.LogInformation("Creating Directory {Dst}", dst);
        Directory.CreateDirectory(dst);
        Directory.GetFiles(src).ForEach(async file =>
        {
            var fileName = ReplaceDnsk(Path.GetFileName(file), pascal, camel);
            var dstFile = Path.Join(dst, fileName);
            _log.LogInformation("Copying {File} to {DstFile}", file, dstFile);
            if (file.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}"))
            {
                // for the git repo files just do direct copy
                File.Copy(file, dstFile);
                return;
            }
            var content = await File.ReadAllTextAsync(file);
            content = ReplaceDnsk(content, pascal, camel);
            await File.WriteAllTextAsync(dstFile, content);
        });
        Directory.GetDirectories(src).Where(x => !isProj || x.Split(Path.DirectorySeparatorChar).Last() is not ("bin" or "obj")).ForEach(
            async dir =>
            {
                var dirName = dir.Split(Path.DirectorySeparatorChar).Last();
                await CopyDir(dir, Path.Join(dst, ReplaceDnsk(dirName, pascal, camel)), pascal, camel, dirName.StartsWith($"{DnskPascal}."));
            });
    }

    private static string ReplaceDnsk(string src, string pascal, string camel)
        => src.Replace(DnskPascal, pascal).Replace(DnskCamel, camel);
}