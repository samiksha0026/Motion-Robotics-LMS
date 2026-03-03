const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5235';

export async function fetchFromBackend(endpoint: string) {
  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) throw new Error('API error');
    
    // Handle empty responses gracefully
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("application/json")) {
      const text = await response.text();
      if (text.trim() === '') {
        return {}; // Return empty object for empty JSON responses
      }
      try {
        return JSON.parse(text);
      } catch (e) {
        throw new Error(`Invalid JSON response: ${text}`);
      }
    } else {
      // Non-JSON response, return as text or empty object
      const text = await response.text();
      return text ? { message: text } : {};
    }
  } catch (error) {
    console.error('Fetch error:', error);
    throw error;
  }
}