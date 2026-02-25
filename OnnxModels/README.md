# ONNX Models for Face Recognition

This directory should contain the ONNX models required for the facial recognition service.

## Required Models

### 1. Face Detection — Ultra-Light-Fast-Generic-Face-Detector-1MB
- **Filename:** `face_detection.onnx`
- **Download:** https://github.com/onnx/models/tree/main/validated/vision/body_analysis/ultraface
- **Direct link:** https://github.com/onnx/models/blob/main/validated/vision/body_analysis/ultraface/models/version-RFB-640.onnx
- **Input:** `[1, 3, 480, 640]` — 480×640 RGB image, normalized to `[-1, 1]`
- **Outputs:** `scores [1, N, 2]` (background/face), `boxes [1, N, 4]` (normalized coords)

### 2. Face Recognition — ArcFace (ResNet100)
- **Filename:** `face_recognition.onnx`
- **Download:** https://github.com/onnx/models/tree/main/validated/vision/body_analysis/arcface
- **Direct link:** https://github.com/onnx/models/blob/main/validated/vision/body_analysis/arcface/model/arcfaceresnet100-11.onnx
- **Input:** `[1, 3, 112, 112]` — 112×112 RGB face image, normalized to `[-1, 1]`
- **Output:** `[1, 512]` — 512-dimensional face embedding

## Setup Instructions

1. Download both model files from the links above.
2. Rename them to `face_detection.onnx` and `face_recognition.onnx`.
3. Place them in this `OnnxModels/` directory.
4. Ensure the paths match `appsettings.json` → `FaceRecognition:DetectionModelPath` and `RecognitionModelPath`.

## Development Mode

If you don't have the ONNX models yet, set `FaceRecognition:UseDevelopmentMode` to `true` in `appsettings.Development.json`. This uses hash-based pseudo-embeddings for testing the full API pipeline without actual facial recognition.

> ⚠️ **Never use development mode in production.** The pseudo-embeddings are deterministic hashes, not real facial features.

## Alternative Models

You can substitute compatible ONNX models as long as they match the expected input/output shapes:
- **Detection:** Any model with `[1, 3, H, W]` input and bounding box + score outputs
- **Recognition:** Any model with `[1, 3, 112, 112]` input and `[1, N]` embedding output

Adjust the settings in `appsettings.json` accordingly.
