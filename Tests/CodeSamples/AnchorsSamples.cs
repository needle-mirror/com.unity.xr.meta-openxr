using UnityEngine.TestTools;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class AnchorsSamples
    {
        public async void AddAnchorAsync_ConvertResultToXrResult()
        {
#region anchors_resultstatus_xrresult
            // This is not optimal. For better performance, reuse a saved reference instead.
            var anchorManager = Object.FindAnyObjectByType<ARAnchorManager>();

            // Create an anchor at an arbitrary pose.
            // You could modify this code to use the position of a raycast hit instead.
            var pose = new Pose(Vector3.zero, Quaternion.identity);
            var result = await anchorManager.TryAddAnchorAsync(pose);

            // To access OpenXR error and success codes, convert status to XrResult
            var xrResult = result.status.AsXrResult();

            // Prints "Success", or the error or success code associated with this operation
            Debug.Log(xrResult);
#endregion
        }
    }
}
