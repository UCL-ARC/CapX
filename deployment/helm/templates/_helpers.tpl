{{- define "capx.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "capx.fullname" -}}
{{- printf "%s-%s" .Release.Name (include "capx.name" .) | trunc 63 | trimSuffix "-" }}
{{- end }}
