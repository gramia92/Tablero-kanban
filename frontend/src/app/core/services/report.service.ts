import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ReportFormat } from '../models/report.model';

@Injectable({ providedIn: 'root' })
export class ReportService {

    constructor(private http: HttpClient) { }

    export(projectId: string, format: ReportFormat): Observable<Blob> {
        return this.http.get(`${environment.apiUrl}/projects/${projectId}/report`, {
            params: { format },
            responseType: 'blob'
        });
    }
}
