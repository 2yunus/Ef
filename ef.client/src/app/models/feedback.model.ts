export interface User {
    id: number;
    username: string;
    email: string;
}

export interface Feedback {
    id: number;
    content: string;
    isAnonymous: boolean;
    category: string;
    sentiment: 'Positive' | 'Neutral' | 'Negative';
    createdAt: string;
    user?: User;
}

export interface FeedbackCategory {
    value: string;
    label: string;
} 