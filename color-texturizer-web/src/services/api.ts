import type { ColorInfo } from '../types/color';

const API_BASE_URL = 'http://localhost:5095/api/image';

export async function extractColors(file: File): Promise<ColorInfo[]> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE_URL}/colors`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw new Error(`Failed to extract colors (${response.status})`);
  }

  return response.json();
}

export async function processImage(file: File): Promise<string> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE_URL}/process`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw new Error(`Failed to process textures (${response.status})`);
  }

  const blob = await response.blob();
  return URL.createObjectURL(blob);
}