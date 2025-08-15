export interface PredictionResult {
  licensePlate: string;
  predictedNextSeen: Date;
  predictedHours: number;
  confidenceScore: number;
  totalHistoricalVisits: number;
  averageTimeBetweenVisits: number;
  lastSeen: Date;
  modelVersion: string;
  predictionMadeAt: Date;
}

export interface PredictionsResponse {
  predictions: PredictionResult[];
}
