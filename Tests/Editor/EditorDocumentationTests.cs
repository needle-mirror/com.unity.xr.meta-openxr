using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine.XR.OpenXR.Features.Meta;

namespace UnityEditor.XR.OpenXR.Features.Meta.Tests
{
    [TestFixture]
    class EditorDocumentationTests
    {
        const string k_PackageName = "com.unity.xr.meta-openxr";

        /// <summary>
        /// <see cref="PackageManager.PackageInfo"/> for xr.sdk.meta-openxr.
        /// </summary>
        PackageManager.PackageInfo m_PackageInfo;

        [OneTimeSetUp]
        public void SetUp()
        {
            m_PackageInfo = PackageManager.PackageInfo.FindForPackageName(k_PackageName);
            Assert.That(m_PackageInfo, Is.Not.Null);
            Assert.That(m_PackageInfo.version, Is.Not.Null);
            Assert.That(m_PackageInfo.version, Is.Not.Empty);
        }

        [Test]
        public void HelpURLVersionMatchesPackageVersion()
        {
            var majorMinorVersionString = GetMajorMinor(m_PackageInfo.version);
            Assert.AreEqual(majorMinorVersionString, Constants.k_MajorMinorVersion);
        }

        static string GetMajorMinor(DependencyInfo dependency) => GetMajorMinor(dependency.version);

        static string GetMajorMinor(string version)
        {
            // Return major.minor from the string.
            // For example, "1.2.3" would return "1.2"
            Assert.That(version, Is.Not.Null);
            Assert.That(version, Is.Not.Empty);
            var secondDotIndex = version.IndexOf('.', version.IndexOf('.') + 1);
            Assert.That(secondDotIndex, Is.GreaterThan(0));
            return version.Substring(0, secondDotIndex);
        }
    }
}
