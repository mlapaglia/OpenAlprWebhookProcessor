# License Plate Machine Learning Module

## Overview

This module provides machine learning capabilities to predict when license plates will next be seen based on historical patterns from ~5 years of data.

## Features

- **Automatic Background Training**: Models are retrained every 6 hours using the latest data
- **Prediction API**: REST endpoints for single and batch predictions
- **Model Persistence**: Trained models are stored in the `config/ml-models/` directory
- **Fallback Predictions**: Heuristic-based predictions when ML models are unavailable
- **Model Versioning**: Automatic backup of previous model versions

## API Endpoints

- `POST /api/machinelearning/predict` - Single license plate prediction
- `POST /api/machinelearning/predict/batch` - Batch predictions (up to 100)
- `GET /api/machinelearning/predict/top` - Top predictions within timeframe
- `GET /api/machinelearning/model/status` - Model availability status
- `POST /api/machinelearning/model/retrain` - Manually trigger training
- `GET /api/machinelearning/model/info` - Model information and features

## Features Used for Predictions

The ML model analyzes these patterns:

- **Time Patterns**: Hour of day, day of week, month, seasonal factors
- **Historical Patterns**: Visit frequency, average time between visits, total visits
- **Context**: Weekend vs weekday, business hours, time since last seen
- **Vehicle Info**: Vehicle type and color (encoded)
- **Location**: Camera ID where the plate was seen

## Model Details

- **Algorithm**: LBFGS Poisson Regression
- **Training Data**: Minimum 100 samples required
- **Quality Threshold**: R² > 0.1 for model acceptance
- **Batch Size**: 50,000 samples per training run
- **Storage**: Models saved as `.zip` files in `config/ml-models/`

## Configuration

Models and configuration data are stored in:
```
config/
└── ml-models/
    ├── license-plate-prediction-model.zip
    └── backup-YYYYMMDD-HHMMSS-license-plate-prediction-model.zip
```

## Getting Started

The ML services are automatically registered and start when the application launches:

1. **Training Service**: Begins training immediately on startup and then every 6 hours
2. **Prediction Service**: Available immediately (uses fallback predictions until model is trained)
3. **API Controller**: All endpoints are secured and require authentication

## Example Usage

```bash
# Get a prediction for a specific license plate
curl -X POST "https://localhost:5001/api/machinelearning/predict" \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{
    "licensePlate": "ABC123",
    "cameraId": 1,
    "lastSeen": "2024-01-15T10:30:00Z",
    "vehicleType": "car",
    "vehicleColor": "white"
  }'

# Response
{
  "licensePlate": "ABC123",
  "predictedNextSeen": "2024-01-16T10:45:00Z",
  "predictedHours": 24.25,
  "confidenceScore": 0.85,
  "totalHistoricalVisits": 45,
  "averageTimeBetweenVisits": 24.5,
  "lastSeen": "2024-01-15T10:30:00Z",
  "modelVersion": "ML.NET-v1.0-2024-01",
  "predictionMadeAt": "2024-01-15T15:00:00Z"
}
```

## Performance Notes

- Initial training may take several minutes with years of data
- Predictions are fast (< 100ms per prediction)
- Background training runs without affecting API performance
- Models automatically improve over time with more data 