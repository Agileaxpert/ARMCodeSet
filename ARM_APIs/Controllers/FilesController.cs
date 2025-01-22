using ARM_APIs.Interface;
using ARMCommon.ActionFilter;
using ARMCommon.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/v{version:apiVersion}")]
[ApiVersion("1")]
[ApiController]
public class FileController : Controller
{
    private readonly IFiles _fileService;

    public FileController(IFiles fileService)
    {
        _fileService = fileService;
    }

    [RequiredFieldsFilter("RecordId", "FilePath")]
    [HttpPost("GetFileByRecordId")]
    public async Task<IActionResult> GetFileByRecordId(FileRequest request)
    {
        try
        {
            var result = await _fileService.GetFileByRecordId(request.RecordId, request.FilePath);


            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Error in fetching data");
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error in fetching data: {ex.Message}");
        }
    }

    [RequiredFieldsFilter("Filename", "FilePath")]
    [HttpPost("GetFileByFileName")]
    public async Task<IActionResult> GetFileByFileName(GetFilerequest request)
    {
        var result = await _fileService.GetFile(request.Filename, request.FilePath);

        if (result != null)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest("Error in fetching data");
        }
    }


    [RequiredFieldsFilter("Filenames", "FilePath")]
    [HttpPost("GetMultipleFilesByFileName")]
    public async Task<IActionResult> GetMultipleFilesByFileName(MultiFiles request)
    {
        var result = await _fileService.GetMultipleFiles(request.Filenames, request.FilePath);

        if (result != null)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest("Error in fetching data");
        }

    }

}
