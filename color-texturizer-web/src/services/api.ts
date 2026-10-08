import type { ColorInfo } from '../types/color';

const API_BASE_URL = 'http://localhost:5095/api/image';

// send image to the server to extract colors
export async function extractColors(file: File): Promise<ColorInfo[]> {
  // create form to send image
  const formData = new FormData();
  formData.append('file', file);

  // send request
  const response = await fetch(`${API_BASE_URL}/colors`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw new Error(`Failed to extract colors (${response.status})`);
  }

  // recieve colors
  return response.json();
}

// process an image
export async function processImage(file: File): Promise<string> {
  // create form to send image
  const formData = new FormData();
  formData.append('file', file);

  // send request
  const response = await fetch(`${API_BASE_URL}/process`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw new Error(`Failed to process textures (${response.status})`);
  }
  
  // create image URL
  const blob = await response.blob();
  return URL.createObjectURL(blob);
}