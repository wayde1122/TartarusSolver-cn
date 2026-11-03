"""Verify both release zips and write dist/SHA256SUMS.txt before publishing."""
import argparse
import hashlib
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path

from package import MODELS, ROOT, VC_RUNTIME


def verify(out: Path) -> None:
    version = ET.parse(ROOT / "Directory.Build.props").findtext(".//Version")
    ocr_files = [item.attrib["Include"] for item in
                 ET.parse(ROOT / "src/Tartarus.Vision/Tartarus.Vision.csproj").findall(".//OcrModel")]
    checksums = []
    for full in (False, True):
        archive = out / f"TartarusSolver-{version}{'-full' if full else ''}.zip"
        with zipfile.ZipFile(archive) as zipped:
            bad = zipped.testzip()
            if bad:
                raise ValueError(f"{archive.name}: corrupt entry {bad}")
            names = set(zipped.namelist())
            required = ["TartarusSolver.exe", "TartarusSolver.dll", "onnxruntime.dll", "README.txt"]
            required += ["models/ocr/" + name for name in ocr_files]
            if full:
                required += ["coreclr.dll", "hostfxr.dll", *VC_RUNTIME]
            for name in required:
                entry = "TartarusSolver/" + name
                if entry not in names or zipped.getinfo(entry).file_size == 0:
                    raise ValueError(f"{archive.name}: missing or empty {name}")
            for pattern in MODELS:
                sources = sorted((ROOT / "models").glob(pattern))
                if not sources:
                    raise ValueError(f"No source models match {pattern}")
                for source in sources:
                    entry = "TartarusSolver/models/" + source.relative_to(ROOT / "models").as_posix()
                    if zipped.read(entry) != source.read_bytes():
                        raise ValueError(f"{archive.name}: model differs from source: {entry}")
        with archive.open("rb") as source:
            digest = hashlib.file_digest(source, "sha256").hexdigest()
        checksums.append(f"{digest}  {archive.name}\n")
        print(f"PASS: {archive.name}")
    (out / "SHA256SUMS.txt").write_text("".join(checksums), encoding="ascii")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--out", type=Path, default=ROOT / "dist")
    verify(parser.parse_args().out)
