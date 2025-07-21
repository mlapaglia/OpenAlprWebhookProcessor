import { HttpClient } from '@angular/common/http'
import { Injectable, inject } from '@angular/core'
import { Observable } from 'rxjs'

export interface ModelStatus {
  modelAvailable: boolean
  status: string
  lastChecked: string
}

export interface ModelInfo {
  modelAvailable: boolean
  modelType: string
  features: string[]
  description: string
  trainingSchedule: string
  lastUpdated: string
}

export interface ModelMetrics {
  rSquared: number
  meanAbsoluteError: number
  rootMeanSquaredError: number
}

export interface ModelFile {
  lastSaved: string
  fileSizeBytes: number
}

export interface TrainingConfiguration {
  trainingInterval: string
  minimumTrainingData: number
  minimumModelQuality: number
  batchSize: number
}

export interface TrainingStatus {
  isTraining: boolean
  lastTrainingStarted?: string
  lastTrainingCompleted?: string
  lastTrainingSuccessful: boolean
  lastError?: string
  trainingDataCount: number
  modelMetrics?: ModelMetrics
  modelFile?: ModelFile
  configuration: TrainingConfiguration
}

@Injectable({
  providedIn: 'root',
})
export class MachineLearningService {
  private http = inject(HttpClient)

  getModelStatus(): Observable<ModelStatus> {
    return this.http.get<ModelStatus>('/api/machinelearning/model/status')
  }

  getModelInfo(): Observable<ModelInfo> {
    return this.http.get<ModelInfo>('/api/machinelearning/model/info')
  }

  getTrainingStatus(): Observable<TrainingStatus> {
    return this.http.get<TrainingStatus>('/api/machinelearning/training/status')
  }

  triggerTraining(): Observable<{ message: string; timestamp: string }> {
    return this.http.post<{ message: string; timestamp: string }>('/api/machinelearning/model/retrain', {})
  }
}
